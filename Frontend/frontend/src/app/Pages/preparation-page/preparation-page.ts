import {ChangeDetectorRef, Component, ViewChild} from '@angular/core';
import {FileUpload} from '../../Components/file-upload/file-upload';
import {HttpClient} from '@angular/common/http';
import {Placeholder} from '../../Components/placeholder/placeholder';
import {PlaceholderConfig} from '../../Components/models/PlaceholderConfig'
import {DataInformation} from '../../Components/data-information/data-information';
import {Preview} from '../../Components/preview/preview';


@Component({
  selector: 'app-preparation-page',
  imports: [
    FileUpload,
    Placeholder,
    DataInformation,
    Preview
  ],
  templateUrl: './preparation-page.html',
  styleUrl: './preparation-page.css',
})
export class PreparationPage {
  configs: PlaceholderConfig[] = [];

  templateTitle: string = "Загрузите шаблон в формате HTML"
  dataTitle: string = "Загрузите данные в формате XLSX"

  template: File | null = null;
  table: File | null = null;

  result: string | null = null;
  variables: string[] = [];
  headers: string[] = [];
  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  total: number = 0;
  previews: {to: string; html: string}[] = [];

  mapping: Map<string, string> = new Map();
  nextFrom: number = 0;

  @ViewChild(DataInformation)
  dataInformation!: DataInformation;

  onPreviewClick() {
    const mapping = this.dataInformation.getMapping();
    this.mapping = mapping;
    const formData = new FormData();
    formData.append('template', this.template!);
    formData.append('table', this.table!);

    formData.append('count', '10');
    formData.append('mappingJson', JSON.stringify(Object.fromEntries(mapping)));
    this.http.post<{emailPreviews: {to: string, html: string}[], nextRow: number, total: number}>('/api/GetPreview', formData).subscribe(response => {
      this.previews = response.emailPreviews;
      this.nextFrom = response.nextRow;
      this.total = response.total;
      console.log(response)
      this.cdr.detectChanges();
    });

  }

  onSendClick() {
    const mapping = this.dataInformation.getMapping();
    this.mapping = mapping;
    const formData = new FormData();
    formData.append('template', this.template!);
    formData.append('table', this.table!);
    formData.append('mappingJson', JSON.stringify(Object.fromEntries(mapping)));
    this.http.post<{emailPreviews: {to: string, html: string}[], nextRow: number, total: number}>('/api/Send', formData).subscribe(response => {
      this.cdr.detectChanges();
    });

  }

  templateReceived(file: File) {
    this.template = file;
    const formData = new FormData();
    formData.append('template', file);
    this.http.post<string[]>('/api/UploadTemplate', formData).subscribe(response => {
      this.variables = response;
      // this.configs = response.map(s => ({
      //   placeholder: s,
      //   offsetX: null,
      //   offsetY: null,
      //   row: false
      // }));
      //this.configs.push({placeholder: "email", offsetX:null, offsetY:null, row:false});
      this.cdr.detectChanges();
    });

  }

  dataReceived(file: File) {
    this.table = file;
    const formData = new FormData();
    formData.append('table', file);
    this.http.post<{headers: string[]}>('/api/ExtractTableHeaders', formData).subscribe(response => {
      this.headers = response.headers;
      this.cdr.detectChanges();
    });
  }
}
