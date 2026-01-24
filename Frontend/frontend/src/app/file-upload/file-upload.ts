import {Component, EventEmitter, Output} from '@angular/core';

@Component({
  selector: 'app-file-upload',
  imports: [],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.css',
})
export class FileUpload {
  file: File | null = null;
  result: string | null = null;

  @Output()
  outputFile: EventEmitter<File> = new EventEmitter<File>();


  fileSelected(event: Event) {
    const element = event.target as HTMLInputElement;
    if (element.files) {
      this.file = element.files[0];
    }
  }

  upload() {
    if (!this.file) {
      this.result = "Файл не выбран"
      return;
    }
    this.outputFile.emit(this.file);
  }
}
