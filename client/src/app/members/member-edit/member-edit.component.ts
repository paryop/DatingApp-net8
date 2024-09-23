import { Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PhotoEditorComponent } from "../photo-editor/photo-editor.component";

@Component({
  selector: 'app-member-edit',
  standalone: true,
  imports: [TabsModule, FormsModule, PhotoEditorComponent],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css'
})
export class MemberEditComponent implements OnInit {
  @HostListener("window:beforeunload",['$event']) notify($event: any){
    if(this.editForm?.dirty){
      $event.returnValue = true;
    }
  }
    @ViewChild("editForm") editForm?: NgForm;
    private accountService = inject(AccountService);
    private memberService = inject(MembersService);
    private toastrService = inject(ToastrService)
    member?: Member;

    ngOnInit(): void {
      this.loadMember()
    }

    loadMember(){
      const username = this.accountService.currentUser()?.username;

      if(!username) return

      this.memberService.getMember(username).subscribe({
        next: m => this.member = m
      })
    }

    updateMember(){
      this.memberService.updateMember(this.editForm?.value).subscribe({
        next: () => {
          this.toastrService.success("Profile updated successfully!!!")
          this.editForm?.reset(this.member)
        }
      })      
    }

    onMemberChange(event: Member){
      this.member = event;
    }
}
