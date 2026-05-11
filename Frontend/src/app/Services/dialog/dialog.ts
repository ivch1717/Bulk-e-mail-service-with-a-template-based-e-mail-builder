import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import { AlertDialogComponent, AlertDialogType } from '../../Components/alert-dialog/alert-dialog';
import { ConfirmDialogComponent } from '../../Components/confirm-dialog/confirm-dialog';

@Injectable({
  providedIn: 'root'
})
export class DialogService {
  constructor(private dialog: MatDialog) {}

  alert(message: string, type: AlertDialogType = 'info', title?: string): Promise<void> {
    const ref = this.dialog.open(AlertDialogComponent, {
      data: { message, type, title },
      width: '420px',
      autoFocus: false,
    });
    return firstValueFrom(ref.afterClosed()).then(() => undefined);
  }

  error(message: string, title?: string): Promise<void> {
    return this.alert(message, 'error', title);
  }

  warning(message: string, title?: string): Promise<void> {
    return this.alert(message, 'warning', title);
  }

  success(message: string, title?: string): Promise<void> {
    return this.alert(message, 'success', title);
  }

  confirm(message: string, title?: string, confirmText?: string, cancelText?: string): Promise<boolean> {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { message, title, confirmText, cancelText },
      width: '420px',
      autoFocus: false,
    });
    return firstValueFrom(ref.afterClosed()).then(result => !!result);
  }
}
