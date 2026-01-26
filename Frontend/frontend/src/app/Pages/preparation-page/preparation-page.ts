import {ChangeDetectorRef, Component} from '@angular/core';
import {FileUpload} from '../../Components/file-upload/file-upload';
import {HttpClient} from '@angular/common/http';
import {Placeholder} from '../../Components/placeholder/placeholder';

@Component({
  selector: 'app-preparation-page',
  imports: [
    FileUpload,
    Placeholder
  ],
  templateUrl: './preparation-page.html',
  styleUrl: './preparation-page.css',
})
export class PreparationPage {
  placeholders: string[] = [];

  templateTitle: string = "Загрузите шаблон в формате HTML"
  dataTitle: string = "Загрузите данные в формате XLSX"

  template: File | null = null;
  data: File | null = null;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  templateReceived(file: File) {
    this.template = file;
    const formData = new FormData();
    formData.append('template', file);
    this.http.post<string[]>('/api/UploadTemplate', formData).subscribe(response => {
      this.placeholders = response;
      this.cdr.detectChanges();
    });
  }

  dataReceived(file: File) {
    this.data = file;
  }
}
