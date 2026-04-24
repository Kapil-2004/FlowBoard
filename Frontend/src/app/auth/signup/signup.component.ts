import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="signup-header">
      <h1>Create account</h1>
      <p>Start organizing your life with FlowBoard today</p>
    </div>

    <form (submit)="onSignup()" #signupForm="ngForm" class="auth-form">
      <div class="form-group">
        <label for="fullName">Full Name</label>
        <input 
          type="text" 
          id="fullName" 
          name="fullName" 
          [(ngModel)]="fullName" 
          required 
          placeholder="John Doe"
          #nameInput="ngModel"
          [class.invalid]="nameInput.touched && nameInput.invalid"
        >
        <span class="error-text" *ngIf="nameInput.touched && nameInput.invalid">
          Full name is required
        </span>
      </div>

      <div class="form-group">
        <label for="email">Email</label>
        <input 
          type="email" 
          id="email" 
          name="email" 
          [(ngModel)]="email" 
          required 
          email 
          placeholder="name@example.com"
          #emailInput="ngModel"
          [class.invalid]="emailInput.touched && emailInput.invalid"
        >
        <span class="error-text" *ngIf="emailInput.touched && emailInput.invalid">
          Please enter a valid email address
        </span>
      </div>

      <div class="form-group">
        <label for="password">Password</label>
        <input 
          type="password" 
          id="password" 
          name="password" 
          [(ngModel)]="password" 
          required 
          minlength="8"
          placeholder="••••••••"
          #passwordInput="ngModel"
          [class.invalid]="passwordInput.touched && passwordInput.invalid"
        >
        <span class="error-text" *ngIf="passwordInput.touched && passwordInput.invalid">
          Password must be at least 8 characters
        </span>
      </div>

      <div class="alert alert-error" *ngIf="errorMessage">
        {{ errorMessage }}
      </div>

      <button type="submit" class="btn-primary" [disabled]="signupForm.invalid || isLoading">
        <span *ngIf="!isLoading">Create account</span>
        <span *ngIf="isLoading" class="loader"></span>
      </button>

      <p class="switch-auth">
        Already have an account? <a routerLink="/login">Sign in</a>
      </p>
    </form>
  `,
  styles: [`
    .signup-header h1 {
      font-family: var(--font-heading);
      font-size: 44px;
      font-weight: 800;
      color: #fff;
      margin-bottom: 8px;
      letter-spacing: -2px;
    }

    .signup-header p {
      color: rgba(255, 255, 255, 0.4);
      margin-bottom: 56px;
      font-size: 16px;
      font-weight: 400;
      letter-spacing: -0.2px;
    }

    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 36px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    label {
      font-size: 11px;
      font-weight: 700;
      color: rgba(255, 255, 255, 0.35);
      text-transform: uppercase;
      letter-spacing: 1.2px;
    }

    input {
      padding: 14px 0;
      border: none;
      border-bottom: 1.5px solid rgba(255, 255, 255, 0.1);
      background-color: transparent !important;
      color: #fff !important;
      font-size: 17px;
      font-weight: 400;
      transition: border-color var(--transition-fast);
      outline: none;
      border-radius: 0;
      width: 100%;
      letter-spacing: -0.2px;
    }

    /* Force transparency on Browser Auto-fill */
    input:-webkit-autofill,
    input:-webkit-autofill:hover, 
    input:-webkit-autofill:focus {
      -webkit-text-fill-color: #fff;
      -webkit-box-shadow: 0 0 0px 1000px transparent inset;
      transition: background-color 5000s ease-in-out 0s;
    }

    input:focus {
      border-color: rgba(255, 255, 255, 0.6);
    }

    input.invalid {
      border-color: var(--color-error);
    }

    .error-text {
      font-size: 10px;
      color: var(--color-error);
      margin-top: 6px;
      text-transform: uppercase;
      letter-spacing: 0.8px;
      font-weight: 600;
    }

    .btn-primary {
      margin-top: 16px;
      padding: 18px;
      background-color: #fff;
      color: #000;
      border-radius: 14px;
      font-weight: 800;
      font-size: 15px;
      font-family: var(--font-heading);
      letter-spacing: -0.2px;
      transition: all var(--transition-fast);
      display: flex;
      justify-content: center;
      align-items: center;
      border: none;
      cursor: pointer;
    }

    .btn-primary:hover:not(:disabled) {
      background-color: #f5f5f5;
      transform: translateY(-2px);
      box-shadow: 0 10px 40px rgba(255, 255, 255, 0.15);
    }

    .btn-primary:disabled {
      opacity: 0.15;
      cursor: not-allowed;
    }

    .switch-auth {
      text-align: center;
      font-size: 14px;
      color: rgba(255, 255, 255, 0.4);
      margin-top: 24px;
      font-weight: 400;
    }

    .switch-auth a {
      color: #fff;
      font-weight: 600;
      margin-left: 6px;
      text-decoration: none;
      border-bottom: 1.5px solid rgba(255, 255, 255, 0.2);
    }

    .alert {
      padding: 18px;
      border-radius: 14px;
      font-size: 14px;
      background-color: rgba(255, 68, 68, 0.05);
      color: var(--color-error);
      border: 1px solid rgba(255, 68, 68, 0.1);
      font-weight: 500;
    }

    .loader {
      width: 18px;
      height: 18px;
      border: 2px solid rgba(0, 0, 0, 0.1);
      border-radius: 50%;
      border-top-color: #000;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class SignupComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  fullName = '';
  email = '';
  password = '';
  isLoading = false;
  errorMessage = '';

  constructor() {}

  onSignup() {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register({ 
      fullName: this.fullName, 
      email: this.email, 
      password: this.password 
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.errorMessage = err.message;
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
