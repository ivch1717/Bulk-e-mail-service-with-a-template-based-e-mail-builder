import { HttpClient } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {ChangeDetectorRef, Component, OnInit} from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { CampaignDetails } from '../../Components/campaign-details/campaign-details';
import {MatButtonModule} from '@angular/material/button';



interface CampaignSummary {
  campaignId: string;
  totalSent: number;
  totalOpened: number;
}

@Component({
  selector: 'app-statistics-page',
  standalone: true,
  imports: [
    MatTableModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    CampaignDetails,
  ],
  templateUrl: './statistics-page.html',
  styleUrl: './statistics-page.css',
})
export class StatisticsPage implements OnInit {
  campaigns: CampaignSummary[] = [];
  loading = true;
  displayedColumns = ['campaignId', 'totalSent', 'totalOpened', 'openRate', 'expand'];
  searchId = '';
  expandedCampaignId: string | null = null;

  constructor(private http: HttpClient, private cdr : ChangeDetectorRef) {}

  ngOnInit() {
    this.http.get<{ campaignSummaries: CampaignSummary[] }>('/api/stats/campaigns').subscribe(response => {
      this.campaigns = response.campaignSummaries;
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  toggleExpand(campaignId: string) {
    this.expandedCampaignId = this.expandedCampaignId === campaignId ? null : campaignId;
  }

  openRate(campaign: CampaignSummary): string {
    if (campaign.totalSent === 0) return '0%';
    return (campaign.totalOpened / campaign.totalSent * 100).toFixed(1) + '%';
  }

  get filteredCampaigns() {
    if (!this.searchId) return this.campaigns;
    return this.campaigns.filter(c => c.campaignId.includes(this.searchId));
  }
}
