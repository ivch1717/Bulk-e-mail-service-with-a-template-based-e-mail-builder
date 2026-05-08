import {ChangeDetectorRef, Component, OnInit, ViewChild} from '@angular/core';
import {FileUpload} from '../../Components/file-upload/file-upload';
import {HttpClient} from '@angular/common/http';
import {Placeholder} from '../../Components/placeholder/placeholder';
import {PlaceholderConfig} from '../../Components/models/PlaceholderConfig'
import {DataInformation} from '../../Components/data-information/data-information';
import {Preview} from '../../Components/preview/preview';
import {TemplateTransferService} from '../../Services/template-transfer/template-transfer';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../Components/confirm-dialog/confirm-dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';


@Component({
  selector: 'app-preparation-page',
  imports: [
    FileUpload,
    Placeholder,
    DataInformation,
    Preview,
    RouterModule,
    MatButtonModule,
    MatIconModule,
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
  constructor(private http: HttpClient, private cdr: ChangeDetectorRef, private templateTransferService: TemplateTransferService, private dialog: MatDialog, private snackBar: MatSnackBar) { }

  total: number = 0;
  previews: {to: string; html: string}[] = [];

  mapping: Map<string, string> = new Map();
  nextFrom: number = 0;

  @ViewChild(DataInformation)
  dataInformation!: DataInformation;

  @ViewChild(Preview)
  preview!: Preview;

  @ViewChild('dataUpload')
  dataUpload!: FileUpload;

  @ViewChild('templateUpload')
  templateUpload!: FileUpload;

  onPreviewClick() {
    const mapping = this.dataInformation.getMapping();
    this.mapping = mapping;
    const formData = new FormData();
    formData.append('template', this.template!);
    formData.append('table', this.table!);

    formData.append('count', '10');
    formData.append('mappingJson', JSON.stringify(Object.fromEntries(mapping)));
    this.http.post<{emailPreviews: {to: string, html: string}[], nextRow: number, total: number}>('/api/preparation/get-preview', formData).subscribe(response => {
      this.previews = response.emailPreviews;
      this.nextFrom = response.nextRow;
      this.total = response.total;
      console.log(response)
      this.cdr.detectChanges();
    });

  }

  onSendClick() {
    if (!this.preview.selectedSmtpId) {
      this.snackBar.open('Выберите SMTP аккаунт для отправки', 'Закрыть', {
        duration: 3000,
      });
      return;
    }
    const smtpId = this.preview.selectedSmtpId;

    let message: string;

    if (!this.preview.subject && !this.preview.campaignName) {
      message = 'Тема письма и название кампании пустые. Письмо может попасть в спам, а кампания будет идентифицирована по ID. Всё равно отправить?';
    } else if (!this.preview.subject) {
      message = 'Тема письма пустая. Письмо с пустой темой может попасть в спам. Всё равно отправить?';
    } else if (!this.preview.campaignName) {
      message = 'Название кампании пустое — в статистике она будет отображаться по ID. Всё равно отправить?';
    } else {
      message = 'Вы уверены, что хотите начать рассылку?';
    }

    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { message },
      width: '350px'
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      const mapping = this.dataInformation.getMapping();
      this.mapping = mapping;
      const formData = new FormData();
      formData.append('template', this.template!);
      formData.append('table', this.table!);
      formData.append('mappingJson', JSON.stringify(Object.fromEntries(mapping)));
      formData.append('subject', this.preview.subject);
      formData.append('smtpId', smtpId);
      formData.append('tracking', String(this.preview.tracking));
      formData.append('campaignName', String(this.preview.campaignName));
      this.http.post<{
        emailPreviews: { to: string, html: string }[],
        nextRow: number,
        total: number
      }>('/api/preparation/send', formData).subscribe(response => {
        this.cdr.detectChanges();
      });
    });
  }

  templateReceived(file: File) {
    this.template = file;
    const formData = new FormData();
    formData.append('template', file);
    this.http.post<{variables:string[]}>('/api/preparation/upload-template', formData).subscribe({
      next: (response) => {
        this.variables = response.variables;
        if (this.variables.length === 1) {
          this.snackBar.open("В шаблоне нет переменных, возможно они обозначены неверно, если так и должно быть это сообщение можно проигнорировать", 'Закрыть', {
            duration: 5000,
          });
        }
        if (this.variables.includes("")) {
          this.snackBar.open("В шаблоне есть переменная без названия", 'Закрыть', {
            duration: 5000,
          });
        }
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.dataInformation.variables = [];
        this.templateUpload.file = null;
        this.snackBar.open(error.error, 'Закрыть', {
          duration: 5000,
        });
        this.cdr.detectChanges();
      }
    });
  }

  dataReceived(file: File) {
    this.table = file;
    const formData = new FormData();
    formData.append('table', file);
    this.http.post<{headers: string[]}>('/api/preparation/extract-table-headers', formData).subscribe({
        next: (response) => {
            this.headers = response.headers;
            this.cdr.detectChanges();
            },
        error: (error) => {
          this.dataInformation.headers = [];
          this.table = null;
          this.dataUpload.file = null;
          this.snackBar.open(error.error, 'Закрыть', {
            duration: 5000,
          });
          this.cdr.detectChanges();
        }
      });
  }

  ngOnInit() {
    const file = this.templateTransferService.templateFile;
    if (file) {
      this.templateReceived(file);
      this.templateTransferService.templateFile = null;
    }
  }
 //test
}
