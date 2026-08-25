import { Component, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, MatToolbarModule, MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent {
  isMobile = signal(window.innerWidth < 700);

  constructor(private authService: AuthService, private router: Router) {}

  @HostListener('window:resize')
  onResize(): void {
    this.isMobile.set(window.innerWidth < 700);
  }

  get userRole(): string | null {
    return this.authService.getRole();
  }

  get fullName(): string | null {
    return this.authService.getFullName();
  }

  createTicket(): void {
    this.router.navigate(['/tickets/new']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}