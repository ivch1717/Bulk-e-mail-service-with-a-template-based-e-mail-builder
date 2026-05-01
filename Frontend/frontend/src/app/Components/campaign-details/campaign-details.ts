import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DatePipe } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartOptions } from 'chart.js';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

interface RecipientInfo {
  email: string;
  openedAt: string;
  userAgent: string | null;
}

interface OpenByHour {
  hour: string;
  count: number;
}

interface CampaignInfo {
  campaignId: string;
  totalSent: number;
  totalOpened: number;
  openRate: number;
  recipients: RecipientInfo[];
  opensByHour: OpenByHour[];
}

@Component({
  selector: 'app-campaign-details',
  imports: [MatTableModule, MatCardModule, MatProgressSpinnerModule, DatePipe, BaseChartDirective],
  templateUrl: './campaign-details.html',
  styleUrl: './campaign-details.css',
})
export class CampaignDetails implements OnInit {
  @Input() campaignId!: string;


  info: CampaignInfo | null = null;
  loading = true;
  displayedColumns = ['email', 'openedAt', 'userAgent'];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.http.get<CampaignInfo>(`/api/stats/campaigns/${this.campaignId}`).subscribe({
      next: (response) => {
        this.info = response;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => this.loading = false
    });
  }

  get chartData(): ChartData<'bar'> {
    return {
      labels: this.info!.opensByHour.map(o =>
        new Date(o.hour).toLocaleTimeString('ru', { hour: '2-digit', minute: '2-digit' })
      ),
      datasets: [{
        label: 'Открытий',
        data: this.info!.opensByHour.map(o => o.count),
        backgroundColor: '#673ab7'
      }]
    };
  }

  chartOptions: ChartOptions<'bar'> = {
    responsive: true,
    plugins: { legend: { display: false } }
  };
}
