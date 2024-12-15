import { AfterViewChecked, Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
import { MessageService } from '../../_services/message.service';
import { Message } from '../../_models/message';
import { TimeAgoPipe } from "../../_pipes/timeago.pipe";
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeAgoPipe, FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent implements AfterViewChecked {  
  // private messageService = inject(MessageService);
  @ViewChild('msgForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scrollContainer: any;
  userName = input.required<string>();
  messagesService = inject(MessageService)
  content = ''
  

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  sendMessage(){
    this.messagesService.sendMessage(this.userName(), this.content).then(() => {
      this.messageForm?.reset();
      this.scrollToBottom();
    }).catch(err => console.log(err))
  }

  private scrollToBottom(){
    if(this.scrollContainer){
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    }
  }
}
