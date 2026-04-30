import { HttpClient } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {ChangeDetectorRef, Component, OnInit} from '@angular/core';
import { MatCardModule } from '@angular/material/card';

interface CampaignSummary {
  campaignId: string;
  totalSent: number;
  totalOpened: number;
}

@Component({
  selector: 'app-statistics-page',
  imports: [
    MatTableModule,
    MatProgressSpinnerModule,
    MatCardModule,
  ],
  templateUrl: './statistics-page.html',
  styleUrl: './statistics-page.css',
})
export class StatisticsPage implements OnInit {
  campaigns: CampaignSummary[] = [];
  loading = true;
  displayedColumns = ['campaignId', 'totalSent', 'totalOpened', 'openRate'];

  constructor(private http: HttpClient, private cdr : ChangeDetectorRef) {}

  ngOnInit() {
    this.http.get<{ campaignSummaries: CampaignSummary[] }>('/api/stats/campaigns').subscribe(response => {
      this.campaigns = response.campaignSummaries;
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  openRate(campaign: CampaignSummary): string {
    if (campaign.totalSent === 0) return '0%';
    return (campaign.totalOpened / campaign.totalSent * 100).toFixed(1) + '%';
  }
}
