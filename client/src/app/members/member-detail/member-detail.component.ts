import { AfterContentInit, AfterViewInit, ChangeDetectorRef, Component, ElementRef, inject, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../_pipes/timeago.pipe';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, DatePipe, TimeAgoPipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit, AfterViewInit {  
  private memberService = inject(MembersService);
  private messageService = inject(MessageService);
  private route = inject(ActivatedRoute);
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

  onTabActivated(data: TabDirective){
    this.activeTab = data;

    if(this.activeTab.heading == 'Messages' && this.messages.length == 0 && this.member){
        this.messageService.getMessageThread(this.member.userName)
      .subscribe({
        next: messages => {
          console.log(messages.length)
          this.messages = messages
        }
      })
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

}
