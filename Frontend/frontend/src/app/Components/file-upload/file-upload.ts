import {Component, EventEmitter, Input, Output} from '@angular/core';

@Component({
  selector: 'app-file-upload',
  imports: [],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.css',
})
export class FileUpload {
  file: File | null = null;

  @Output()
  outputFile: EventEmitter<File> = new EventEmitter<File>();

  @Input()
  title: string = ""

  fileSelected(event: Event) {
    const element = event.target as HTMLInputElement;
    if (element.files) {
      this.file = element.files[0];
    }
  }

  upload() {
    if (!this.file) {
      return;
    }
    this.outputFile.emit(this.file);
  }
}
