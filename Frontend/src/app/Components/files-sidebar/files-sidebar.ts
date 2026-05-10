import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FileSummary } from '../../Services/files/files';

@Component({
  selector: 'app-files-sidebar',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './files-sidebar.html',
  styleUrl: './files-sidebar.css',
})
export class FilesSidebar {
  @Input() files: FileSummary[] = [];
  @Input() loading: boolean = false;
  @Output() fileDelete = new EventEmitter<FileSummary>();
  @Output() fileLoad = new EventEmitter<FileSummary>();
}
