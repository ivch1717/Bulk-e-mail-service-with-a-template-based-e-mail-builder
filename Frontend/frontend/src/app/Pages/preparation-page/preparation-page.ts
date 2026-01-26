import {ChangeDetectorRef, Component} from '@angular/core';
import {FileUpload} from '../../Components/file-upload/file-upload';
import {HttpClient} from '@angular/common/http';
import {Placeholder} from '../../Components/placeholder/placeholder';
import {PlaceholderConfig} from '../../Components/models/PlaceholderConfig'

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
  configs: PlaceholderConfig[] = [];

  templateTitle: string = "Загрузите шаблон в формате HTML"
  dataTitle: string = "Загрузите данные в формате XLSX"

  template: File | null = null;
  data: File | null = null;

  result: string | null = null;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  templateReceived(file: File) {
    this.template = file;
    const formData = new FormData();
    formData.append('template', file);
    this.http.post<string[]>('/api/UploadTemplate', formData).subscribe(response => {
      this.configs = response.map(s => ({
        placeholder: s,
        offsetX: null,
        offsetY: null,
        row: false
      }));
      //this.configs.push({placeholder: "email", offsetX:null, offsetY:null, row:false});
      this.cdr.detectChanges();
    });

  }

  dataReceived(file: File) {
    this.data = file;
  }

  console() {
    const formData = new FormData();
    formData.append('template', this.template!) // TODO: Убрать !
    formData.append('data', this.data!)
    formData.append('tableInfosJson', JSON.stringify(this.configs))
    this.http.post('/api/ProcessEmailCreation', formData).subscribe(response => {
      this.result = JSON.stringify(response);
      this.cdr.detectChanges();
    });
  }
}
