import {Component, EventEmitter, Input, Output} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';


@Component({
  standalone: true,
  selector: 'app-file-upload',
  imports: [MatButtonModule, MatIconModule],
  templateUrl: './file-upload.html'
})
export class FileUpload {
  file: File | null = null;

  @Output()
  outputFile: EventEmitter<File> = new EventEmitter<File>();

  @Input()
  file_type: string = ""

  fileSelected(event: Event) {
    const element = event.target as HTMLInputElement;
    if (element.files) {
      this.file = element.files[0];
      this.upload();
    }
  }

  upload() {
    if (!this.file) {
      return;
    }
    this.outputFile.emit(this.file);
  }
}
