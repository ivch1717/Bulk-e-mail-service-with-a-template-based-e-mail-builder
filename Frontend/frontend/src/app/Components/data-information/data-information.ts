import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';

@Component({
  selector: 'app-data-information',
  imports: [],
  templateUrl: './data-information.html',
  styleUrl: './data-information.css',
})
export class DataInformation implements OnChanges{
  @Input()
  variables: string[] = [];

  @Input()
  headers: string[] = [];

  mapping: Map<string, string> = new Map<string, string>();

  ngOnChanges(changes: SimpleChanges) {
    this.processMapping();
  }

  processMapping() {
    for (let v of this.variables) {
      if (this.headers.includes(v)) {
        this.mapping.set(v, v);
      }
    }
  }
}
