import {ChangeDetectorRef, Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DomSanitizer, SafeHtml} from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-preview',
  imports: [FormsModule],
  templateUrl: './preview.html',
  styleUrl: './preview.css',
})
export class Preview implements OnChanges {

  constructor(private sanitizer: DomSanitizer, private http: HttpClient, private cdr: ChangeDetectorRef) { }

  @Input()
  total: number = 0;

  @Input()
  previews: {to: string; html: string}[] = [];

  @Input()
  template: File | null = null;

  @Input()
  table: File | null = null;

  @Input()
  mapping: Map<string, string> = new Map();

  @Input()
  nextFrom: number = 0;

  index: number = 0;
  currentPreview: {to: string; html: string} = {to: "", html: ""};
  subject: string = "";

  get safeHtml(): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(this.currentPreview.html);
  }

  next() {
    if (this.index == this.total - 1) {
      return;
    }
    this.index++;
    if (this.index >= this.previews.length) {
      const formData = new FormData();
      formData.append('template', this.template!);
      formData.append('table', this.table!);
      formData.append('from', String(this.nextFrom));
      formData.append('count', '10');
      formData.append('mappingJson', JSON.stringify(Object.fromEntries(this.mapping)));
      this.http.post<{emailPreviews: {to: string, html: string}[], nextRow: number}>('/api/GetPreview', formData).subscribe(response => {
        this.previews.push(...response.emailPreviews);
        this.nextFrom = response.nextRow;
        this.currentPreview = this.previews[this.index];
        this.cdr.detectChanges();
      });
    } else {
      this.currentPreview = this.previews[this.index];
    }
  }

  prev() {
    if (this.index == 0) {
      return;
    }
    this.index--;
    this.currentPreview = this.previews[this.index];
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['previews'] && this.previews.length > 0) {
      this.index = 0;
      this.currentPreview = this.previews[0];
    }
  }
}
