import { AfterContentInit, AfterViewInit, ChangeDetectorRef, Component, ElementRef, inject, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../_pipes/timeago.pipe';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountService } from '../../_services/account.service';
import { HubConnectionState } from '@microsoft/signalr';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, DatePipe, TimeAgoPipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit, AfterViewInit,OnDestroy {  
  private memberService = inject(MembersService);
  private messageService = inject(MessageService);
  private accountService = inject(AccountService);
  presenceService = inject(PresenceService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  @ViewChild('memberTabs', {static: false}) memberTabs!: TabsetComponent;

  member?: Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  messages: Message[] = []

  /**
   *
   */
  constructor(private cdr: ChangeDetectorRef) {
        
  }
  
  
  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member']
        this.member && this.member.photos.map(p => {
          this.images.push(new ImageItem({src: p.url, thumb: p.url}))
        })
      }
    })

    this.route.paramMap.subscribe({
      next: _ => this.onRouterParamsChange()
    })
  }  

  ngAfterViewInit (): void {    
    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })
    this.cdr.detectChanges()
  }

    selectTab(heading: string){
    if(this.memberTabs) {
      const messageTab = this.memberTabs.tabs?.find(x=> x.heading == heading);

      if(messageTab) messageTab.active = true;
    }
  }

  updateMessages(event: Message){
    this.messages.push(event);
  }

  onRouterParamsChange(){
    const user = this.accountService.currentUser();

    if(!user) return;

    if(this.messageService.hubConnection?.state == HubConnectionState.Connected && 
      this.activeTab?.heading == "Messages") {
        this.messageService.hubConnection.stop();
        this.messageService.createHubConnection(user, this.member?.userName!);
      }
  }

  onTabActivated(data: TabDirective){
    this.activeTab = data;

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {tab: this.activeTab.heading},
      queryParamsHandling: 'merge'
    });

    if(this.activeTab.heading == 'Messages' && this.member){
      const user = this.accountService.currentUser();

      if(!user) return;

      this.messageService.createHubConnection(user, this.member.userName);
      //   this.messageService.getMessageThread(this.member.userName)
      // .subscribe({
      //   next: messages => {
      //     this.messages = messages
      //   }
      // })
    } else {
      this.messageService.stopHubConnection();
    }
  }

  loadMembers(){
    const username = this.route.snapshot.paramMap.get('username');

    if(!username) return;

    this.memberService.getMember(username).subscribe({
      next: member => {
        this.member = member;
        member.photos.map(p => {
          this.images.push(new ImageItem({
            src: p.url, thumb: p.url
          }))
        })
      }
    })
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
