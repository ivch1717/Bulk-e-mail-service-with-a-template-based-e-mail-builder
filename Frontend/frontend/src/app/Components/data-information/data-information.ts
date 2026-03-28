import {Component, Input} from '@angular/core';

@Component({
  selector: 'app-data-information',
  imports: [],
  templateUrl: './data-information.html',
  styleUrl: './data-information.css',
})
export class DataInformation {
  @Input()
  variables: string[] = [];
}
