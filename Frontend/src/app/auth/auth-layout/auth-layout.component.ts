import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <div class="auth-container">
      <div class="auth-left">
        <div class="auth-form-card">
          <div class="logo">
            <span class="logo-icon">≋</span>
            <span class="logo-text">FlowBoard</span>
          </div>
          <router-outlet></router-outlet>
        </div>
      </div>
      <div class="auth-right">
        <div class="art-overlay"></div>
        <div class="art-content">
          <h1>Effortless<br>Productivity.</h1>
          <p>FlowBoard helps you organize your tasks with a minimalist and aesthetic touch.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      display: flex;
      min-height: 100vh;
      background: radial-gradient(circle at 10% 10%, #111 0%, #000 50%); /* Subtle depth */
      color: #fff;
      font-family: var(--font-body);
      position: relative;
      overflow: hidden;
    }

    .auth-left {
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      padding: 40px;
      z-index: 10;
    }

    .auth-form-card {
      width: 100%;
      max-width: 480px;
      padding: 64px;
      background: rgba(255, 255, 255, 0.02);
      backdrop-filter: blur(20px);
      border: 1px solid rgba(255, 255, 255, 0.05);
      border-radius: 32px;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 48px;
    }

    .logo-icon {
      font-size: 20px;
      color: #fff;
      opacity: 0.9;
    }

    .logo-text {
      font-family: var(--font-heading);
      font-size: 19px;
      font-weight: 700;
      letter-spacing: -0.8px;
      opacity: 0.95;
    }

    .auth-right {
      flex: 1.2;
      display: flex;
      align-items: center;
      justify-content: center;
      padding-right: 5%;
      position: relative;
    }

    .art-content {
      width: 92%;
      height: 82%;
      background: url('/assets/images/minimalist-art.png') center/cover no-repeat;
      border-radius: 40px;
      border: 1px solid rgba(255, 255, 255, 0.08);
      box-shadow: 0 50px 100px rgba(0,0,0,0.9);
      position: relative;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      justify-content: flex-end;
      padding: 64px;
    }

    .art-overlay {
      position: absolute;
      inset: 0;
      background: linear-gradient(180deg, rgba(0, 0, 0, 0) 30%, rgba(0, 0, 0, 0.95) 100%);
    }

    .art-text {
      position: relative;
      z-index: 5;
    }

    .art-text h1 {
      font-family: var(--font-heading);
      font-size: 52px;
      font-weight: 800;
      line-height: 1.05;
      margin-bottom: 20px;
      letter-spacing: -2.5px;
    }

    .art-text p {
      font-size: 17px;
      color: rgba(255, 255, 255, 0.5);
      font-weight: 400;
      max-width: 340px;
      line-height: 1.6;
      letter-spacing: -0.2px;
    }

    @media (max-width: 1024px) {
      .auth-container { flex-direction: column; padding: 40px 20px; }
      .auth-left { padding: 0; margin-bottom: 60px; }
      .auth-form-card { padding: 40px; }
      .auth-right { padding: 0; height: 500px; }
      .art-content { height: 100%; width: 100%; }
    }
  `]
})
export class AuthLayoutComponent {}
