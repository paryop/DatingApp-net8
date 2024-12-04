import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';
import { tick } from '@angular/core/testing';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private modalService = inject(BsModalService);
  users: User[] = [];
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  openRolesModal(user: User){
    var r: string[] = [];

    for(var role of user.roles) {
      r.push(role.name ? role.name : role);
    }    

    const initialState: ModalOptions = {
      class: "modal-lg",
      initialState: {
        title: 'User  Roles',
        username: user.username,
        availableRoles: ['Admin', 'Moderator', 'Member'],
        selectedRoles: r,
        users: this.users,
        rolesUpdated: false
      }
    }

    this.bsModalRef = this.modalService.show(RolesModalComponent, initialState);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        if(this.bsModalRef.content && this.bsModalRef.content.rolesUpdated){
          const selectedRoles = this.bsModalRef.content.selectedRoles;
          this.adminService.updateEditRoles(user.username, selectedRoles.join(",")).subscribe({
            next: 
              roles => {                
                user.roles = roles
                this.updateUsers(user)
              }            
          })
        }
      }
    })
  }

  updateUsers(user: User){
    var idx = this.users.findIndex(obj => obj.username == user.username);
    this.users[idx] = user
  }

  getUsersWithRoles(){
    this.adminService.getUserWithRoles().subscribe({
      next: users => {
        this.users = users
      }
    })   
  }

  getRoles(roles: any[]){
      var r = '';
      for(var role of roles) {
        if(role.name)
          r = r + role.name + ',' 
        else
          r = r + role + ','  
      }
      return r.substring(0, r.length-1);
  }
}
