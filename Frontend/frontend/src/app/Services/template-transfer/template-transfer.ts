import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TemplateTransferService {
  templateFile: File | null = null;
}
