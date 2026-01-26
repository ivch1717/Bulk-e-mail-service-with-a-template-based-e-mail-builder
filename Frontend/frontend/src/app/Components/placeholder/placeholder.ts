import {Component, Input} from '@angular/core';
import {PlaceholderConfig} from "../models/PlaceholderConfig"

@Component({
  selector: 'app-placeholder',
  imports: [],
  templateUrl: './placeholder.html',
  styleUrl: './placeholder.css',
})
export class Placeholder {
  @Input()
  config!: PlaceholderConfig

  rowChanged($event: Event) {
    const target = $event.target as HTMLInputElement;
    this.config.offsetX = Number(target.value) - 1 // чиним индекцсацию
  }

  columnChanged($event: Event) {
    const target = $event.target as HTMLInputElement;
    this.config.offsetY = Number(target.value) - 1 // чиним индекцсацию
  }

  modeChanged($event: Event) {
    const target = $event.target as HTMLInputElement;
    this.config.row = Boolean(target.checked)
  }
}
