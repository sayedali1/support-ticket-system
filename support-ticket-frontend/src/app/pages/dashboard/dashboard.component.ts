import { Component, OnInit, signal, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { Chart, registerables } from 'chart.js';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardStats } from '../../models/dashboard.model';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit, AfterViewInit {
  @ViewChild('statusChart') statusChartRef!: ElementRef<HTMLCanvasElement>;

  stats = signal<DashboardStats | null>(null);
  isLoading = signal(true);
  errorMessage = signal('');

  agentWorkloadColumns: string[] = ['agentName', 'assignedTicketCount', 'openAssignedCount'];

  private chart: Chart | null = null;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadStats();
  }

  ngAfterViewInit(): void {
    // Chart is rendered once data arrives (see loadStats -> renderChart)
  }

  loadStats(): void {
    this.isLoading.set(true);

    this.dashboardService.getStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.isLoading.set(false);
        setTimeout(() => this.renderChart(data), 0); // wait for canvas to exist in DOM
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to load dashboard.');
        this.isLoading.set(false);
      },
    });
  }

  private renderChart(data: DashboardStats): void {
    if (!this.statusChartRef) return;

    if (this.chart) {
      this.chart.destroy();
    }

    this.chart = new Chart(this.statusChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels: data.statusBreakdown.map((s) => s.status),
        datasets: [
          {
            label: 'Tickets by Status',
            data: data.statusBreakdown.map((s) => s.count),
            backgroundColor: ['#3f51b5', '#ff9800', '#4caf50', '#9e9e9e'],
          },
        ],
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: false },
        },
        scales: {
          y: { beginAtZero: true, ticks: { stepSize: 1 } },
        },
      },
    });
  }
}