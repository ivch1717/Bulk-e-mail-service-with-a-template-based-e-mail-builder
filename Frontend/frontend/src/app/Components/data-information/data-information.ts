import {Component, EventEmitter, Input, OnChanges, Output, SimpleChanges} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {MatTableModule} from '@angular/material/table';
import {MatSelectModule} from '@angular/material/select';
import {MatFormFieldModule} from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';


@Component({
  selector: 'app-data-information',
  standalone: true,
  imports: [FormsModule, MatTableModule, MatSelectModule, MatFormFieldModule, MatCardModule],
  templateUrl: './data-information.html',
  styleUrl: './data-information.css'
})

export class DataInformation implements OnChanges{
  @Input()
  variables: string[] = [];

  @Input()
  headers: string[] = [];

  displayedColumns = ['variable', 'header'];

  mapping: Map<string, string> = new Map<string, string>();

  ngOnChanges(changes: SimpleChanges) {
    this.processMapping();
  }

  getMapping(): Map<string, string> {
    return this.mapping;
  }

  processMapping() {
    for (let v of this.variables) {
      if (this.headers.includes(v)) {
        this.mapping.set(v, v);
      }
    }
  }

  onMappingChange(variable: string, value: string) {
    this.mapping.set(variable, value);
  }
}
