import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  url = environment.apiUrl //"https://localhost:5001/api/account/"
  httpClient = inject(HttpClient);
  currentUser = signal<User | null>(null);
  login(model: any){
    return this.httpClient.post<User>(this.url + "account/login", model).pipe(
      map(user => {
        if(user){
          localStorage.setItem("user", JSON.stringify(user));
          this.currentUser.set(user); 
        }
      })
    );    
  }

  register(model: any){
    return this.httpClient.post<User>(this.url + "account/register", model).pipe(
      map(user => {
        if(user){
          localStorage.setItem("user", JSON.stringify(user));
          this.currentUser.set(user); 
        }
        return user;
      })
    ); 
  }

  logout(){
    localStorage.removeItem("user");
    this.currentUser.set(null); 
  }

  constructor() { }
}
