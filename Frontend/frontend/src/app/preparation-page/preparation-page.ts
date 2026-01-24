import { Component } from '@angular/core';
import {FileUpload} from '../file-upload/file-upload';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-preparation-page',
  imports: [
    FileUpload
  ],
  templateUrl: './preparation-page.html',
  styleUrl: './preparation-page.css',
})
export class PreparationPage {
  result: string | null = null;

  constructor(private http: HttpClient) {}

  fileReceived(file: File) {
    const formData = new FormData();
    formData.append('template', file);
    this.http.post('/api/UploadTemplate', formData, {
      responseType: 'text'
    }).subscribe(response => {this.result = response;});
  }
}
