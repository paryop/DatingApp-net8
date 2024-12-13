import { Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
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
export class MemberMessagesComponent {
  // private messageService = inject(MessageService);
  @ViewChild('msgForm') messageForm?: NgForm;
  userName = input.required<string>();
  // messages = input.required<Message[]>();
  messagesService = inject(MessageService)
  // updateMessages = output<Message>();
  content = ''
  // ngOnInit(): void {
  //   this.loadMessages();
  // }

  // loadMessages(){
  //   this.messageService.getMessageThread(this.userName())
  //     .subscribe({
  //       next: messages => this.messages = messages
  //     })
  // }

  sendMessage(){
    this.messagesService.sendMessage(this.userName(), this.content).then(() => {
      this.messageForm?.reset();
    }).catch(err => console.log(err))
}
}
