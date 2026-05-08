import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SmtpService, SmtpProfile, CreateSmtpProfile } from '../../Services/smtp/smtp';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  templateUrl: './config-page.html',
  styleUrl: './config-page.css',
})
export class ConfigPage implements OnInit {
  profiles: SmtpProfile[] = [];
  loadProfilesFailed = false;

  host = '';
  port = 587;
  user = '';
  password = '';
  fromEmail = '';
  displayName = '';

  constructor(
    private smtp: SmtpService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadProfiles();
  }

  loadProfiles() {
    this.loadProfilesFailed = false;

    this.smtp.getProfiles().subscribe({
      next: (profiles) => {
        this.profiles = profiles;
        this.cdr.detectChanges();
      },
      error: () => {
        this.profiles = [];
        this.loadProfilesFailed = true;
        this.snackBar.open('Не удалось загрузить профили', 'Закрыть', { duration: 3000 });
        this.cdr.detectChanges();
      }
    });
  }

  add() {
    const profile: CreateSmtpProfile = {
      host: this.host,
      port: this.port,
      user: this.user,
      password: this.password,
      fromEmail: this.fromEmail,
      displayName: this.displayName
    };

    this.smtp.addProfile(profile).subscribe({
      next: () => {
        this.snackBar.open('Профиль добавлен', 'Закрыть', { duration: 3000 });
        this.loadProfiles();
      },
      error: () => this.snackBar.open('Ошибка при добавлении', 'Закрыть', { duration: 3000 })
    });
  }

  delete(id: string) {
    this.smtp.deleteProfile(id).subscribe({
      next: () => {
        this.snackBar.open('Профиль удалён', 'Закрыть', { duration: 3000 });
        this.loadProfiles();
      },
      error: () => this.snackBar.open('Ошибка при удалении', 'Закрыть', { duration: 3000 })
    });
  }
}
