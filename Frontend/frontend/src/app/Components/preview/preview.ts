import {ChangeDetectorRef, Component, Input} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DomSanitizer, SafeHtml} from '@angular/platform-browser';
@Component({
  selector: 'app-preview',
  imports: [],
  templateUrl: './preview.html',
  styleUrl: './preview.css',
})
export class Preview {

  constructor(private sanitizer: DomSanitizer, private http: HttpClient, private cdr: ChangeDetectorRef) { }

  @Input()
  total: number = 0;

  @Input()
  previews: {to: string; html: string}[] = [];

  index: number = 0;
  currentPreview: {to: string; html: string} = {to: "", html: ""};

  get safeHtml(): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(this.currentPreview.html);
  }

  next() {
    if (this.index == this.total) {
      return;
    }
    this.index++;
    if (this.index >= this.previews.length) {
      // const formData = new FormData();
      // formData.append('table', blablabla);
      // this.http.post('/api/blablabla', formData).subscribe(response => {
      //   this.previews.concat(response);
      //   this.currentPreview = this.previews[this.index];
      //   this.cdr.detectChanges();
      // });
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

}
