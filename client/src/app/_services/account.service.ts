import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  url = environment.apiUrl //"https://localhost:5001/api/account/"
  private likeService = inject(LikesService);
  private presenceService = inject(PresenceService);
  httpClient = inject(HttpClient);
  currentUser = signal<User | null>(null);
  roles = computed(() => {
    const user = this.currentUser();
    if(user && user.token) {
      const role = JSON.parse(atob(user.token.split('.')[1])).role;
      return Array.isArray(role) ? role : [role];
    }
    return null;
  })


  login(model: any){
    return this.httpClient.post<User>(this.url + "account/login", model).pipe(
      map(user => {
        if(user){
          this.setCurrentUser(user);
        }
      })
    );    
  }

  register(model: any){
    return this.httpClient.post<User>(this.url + "account/register", model).pipe(
      map(user => {
        if(user){          
          this.setCurrentUser(user);
        }
        return user;
      })
    ); 
  }

  logout(){
    localStorage.removeItem("user");
    this.currentUser.set(null); 
    this.presenceService.stopHubConnection();
  }

  setCurrentUser(user: User){
      localStorage.setItem("user", JSON.stringify(user));
      this.currentUser.set(user); 
      this.likeService.getLikeIds();
      this.presenceService.createConnectionHub(user);
  }

  constructor() { }
}
