import { Component, inject, input, output } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountService);
  cancelRegister = output<boolean>();

  model: any = {};

  register(){
    this.accountService.register(this.model)
    .subscribe({
      next: (res)=> {
        console.log(res)
        this.cancel()
      },
      error: err => console.log(err.error)
    })
  }

  cancel(){
    this.cancelRegister.emit(false);
  }    
}

  

