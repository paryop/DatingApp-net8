import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  standalone: true,
  imports: [],
  templateUrl: './test-errors.component.html',
  styleUrl: './test-errors.component.css'
})
export class TestErrorsComponent {
  baseUrl = "https://localhost:5001/api/";
  httpClient = inject(HttpClient);
  eMessage: string = '';
  validationErrors: [] = [];

  get400Error(){
    this.httpClient.get(this.baseUrl + "buggy/bad-request").subscribe(
      {
        next: res => console.log(res),
        error: err => this.eMessage = err.error
      })
  }

  get401Error(){
    this.httpClient.get(this.baseUrl + "buggy/auth").subscribe(
      {
        next: res => console.log(res),
        error: err => {
          console.log(err)
        }
      })
  }

  get404Error(){
    this.httpClient.get(this.baseUrl + "buggy/not-found").subscribe(
      {
        next: res => console.log(res),
        error: err => {
          console.log(err)
        }
      })
  }

  get500Error(){
    this.httpClient.get(this.baseUrl + "buggy/server-error").subscribe(
      {
        next: res => console.log(res),
        error: err => console.log(err)
      })
  }

  get400ValidationErrors(){
    this.httpClient.post(this.baseUrl + "account/register",{}).subscribe(
      {
        next: res => console.log(res),
        error: err => this.validationErrors = err
      })
  }
}
