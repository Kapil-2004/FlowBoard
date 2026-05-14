import { Component, inject, OnInit, signal, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, AdminDashboardDto, PlatformAnalyticsDto, AuditLogDto, ActivityReportDto } from '../../services/admin.service';
import { UserDto } from '../../models/auth.models';

type Tab = 'users'|'workspaces'|'boards'|'analytics'|'notify'|'audit'|'report';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
<div class="ap">
  <div class="ap-header">
    <div class="ap-title"><span>🛡️</span><div><h2>Platform Admin</h2><p>Full system control</p></div></div>
    <button class="btn-x" (click)="onClose.emit()">✕ Close</button>
  </div>

  <!-- Stats row -->
  <div class="stats" *ngIf="stats()">
    <div class="stat"><b>{{stats()!.totalUsers}}</b><span>Users</span></div>
    <div class="stat g"><b>{{stats()!.activeUsers}}</b><span>Active</span></div>
    <div class="stat r"><b>{{stats()!.suspendedUsers}}</b><span>Suspended</span></div>
    <div class="stat b"><b>{{stats()!.totalWorkspaces}}</b><span>Workspaces</span></div>
    <div class="stat p"><b>{{stats()!.totalBoards}}</b><span>Boards</span></div>
    <div class="stat o"><b>{{stats()!.overdueCards}}</b><span>Overdue</span></div>
  </div>

  <!-- Tabs -->
  <div class="tabs">
    <button *ngFor="let t of tabList" [class.active]="tab===t.key" (click)="switchTab(t.key)">{{t.label}}</button>
  </div>

  <!-- Toast -->
  <div class="toast" *ngIf="toast()" [class]="'toast '+toastType()">{{toast()}}</div>

  <!-- TAB: USERS -->
  <div *ngIf="tab==='users'">
    <div class="filter-row">
      <input id="us-search" class="inp" placeholder="🔍 Search..." [(ngModel)]="uq">
      <select id="us-role" class="sel" [(ngModel)]="uRole"><option value="">All Roles</option><option>Member</option><option>BoardAdmin</option><option>PlatformAdmin</option></select>
      <select id="us-status" class="sel" [(ngModel)]="uStatus"><option value="">All</option><option value="active">Active</option><option value="suspended">Suspended</option></select>
    </div>
    <div class="loading" *ngIf="loadingUsers()">Loading...</div>
    <table class="tbl" *ngIf="!loadingUsers()">
      <thead><tr><th>User</th><th>Email</th><th>Role</th><th>Status</th><th>Last Login</th><th>Actions</th></tr></thead>
      <tbody>
        <tr *ngFor="let u of filteredUsers()" [class.dim]="!u.isActive">
          <td class="uc"><div class="av">{{ini(u.fullName)}}</div>{{u.fullName}}</td>
          <td class="sec">{{u.email}}</td>
          <td><span class="rb role-{{u.role.toLowerCase()}}">{{u.role}}</span></td>
          <td><span class="sb" [class.active]="u.isActive" [class.sus]="!u.isActive">{{u.isActive?'Active':'Suspended'}}</span></td>
          <td class="sec">{{u.lastLoginAt?(u.lastLoginAt|date:'MMM d HH:mm'):'Never'}}</td>
          <td class="ac">
            <select [id]="'role-'+u.userId" class="rsel" [ngModel]="u.role" (ngModelChange)="chRole(u,$event)" [disabled]="isPrimary(u)">
              <option>Member</option><option>BoardAdmin</option><option>PlatformAdmin</option>
            </select>
            <button *ngIf="u.isActive&&!isPrimary(u)" [id]="'sus-'+u.userId" class="ba sus-btn" (click)="sus(u)">⏸</button>
            <button *ngIf="!u.isActive" [id]="'act-'+u.userId" class="ba act-btn" (click)="act(u)">▶</button>
            <button *ngIf="!isPrimary(u)" [id]="'del-'+u.userId" class="ba del-btn" (click)="del(u)">🗑</button>
          </td>
        </tr>
        <tr *ngIf="filteredUsers().length===0"><td colspan="6" class="empty">No users match.</td></tr>
      </tbody>
    </table>
  </div>

  <!-- TAB: WORKSPACES -->
  <div *ngIf="tab==='workspaces'">
    <button class="btn-refresh" (click)="loadWorkspaces()">↻ Refresh</button>
    <div class="loading" *ngIf="loadingWS()">Loading workspaces...</div>
    <table class="tbl" *ngIf="!loadingWS()">
      <thead><tr><th>ID</th><th>Name</th><th>Owner</th><th>Visibility</th><th>Action</th></tr></thead>
      <tbody>
        <tr *ngFor="let w of workspaces()">
          <td>{{w.workspaceId??w.id??'-'}}</td>
          <td>{{w.name}}</td>
          <td>{{w.ownerId??w.ownerName??'-'}}</td>
          <td><span class="rb">{{w.visibility??'PUBLIC'}}</span></td>
          <td><button class="ba del-btn" (click)="delWS(w)">🗑 Delete</button></td>
        </tr>
        <tr *ngIf="workspaces().length===0"><td colspan="5" class="empty">No workspaces found.</td></tr>
      </tbody>
    </table>
  </div>

  <!-- TAB: BOARDS -->
  <div *ngIf="tab==='boards'">
    <button class="btn-refresh" (click)="loadBoards()">↻ Refresh</button>
    <div class="loading" *ngIf="loadingBoards()">Loading boards...</div>
    <table class="tbl" *ngIf="!loadingBoards()">
      <thead><tr><th>ID</th><th>Name</th><th>Visibility</th><th>Closed</th><th>Action</th></tr></thead>
      <tbody>
        <tr *ngFor="let b of boards()">
          <td>{{b.boardId??b.id??'-'}}</td>
          <td>{{b.name}}</td>
          <td><span class="rb">{{b.visibility??'-'}}</span></td>
          <td><span class="sb" [class.sus]="b.isClosed">{{b.isClosed?'Closed':'Active'}}</span></td>
          <td><button class="ba del-btn" (click)="delBoard(b)">🗑 Delete</button></td>
        </tr>
        <tr *ngIf="boards().length===0"><td colspan="5" class="empty">No boards found.</td></tr>
      </tbody>
    </table>
  </div>

  <!-- TAB: ANALYTICS -->
  <div *ngIf="tab==='analytics'">
    <button class="btn-refresh" (click)="loadAnalytics()">↻ Refresh</button>
    <div class="loading" *ngIf="loadingAn()">Loading analytics...</div>
    <div *ngIf="analytics() && !loadingAn()">
      <div class="stats" style="margin-bottom:16px">
        <div class="stat"><b>{{analytics()!.totalUsers}}</b><span>Users</span></div>
        <div class="stat g"><b>{{analytics()!.activeUsers}}</b><span>Active</span></div>
        <div class="stat b"><b>{{analytics()!.newUsersThisWeek}}</b><span>New This Week</span></div>
        <div class="stat p"><b>{{analytics()!.newUsersThisMonth}}</b><span>New This Month</span></div>
      </div>
      <div class="section-title">Role Distribution</div>
      <div class="dist-row">
        <div *ngFor="let kv of roleEntries()" class="dist-card">
          <b>{{kv[1]}}</b><span>{{kv[0]}}</span>
        </div>
      </div>
      <div class="section-title" style="margin-top:16px">Recent Logins</div>
      <table class="tbl">
        <thead><tr><th>Email</th><th>Role</th><th>Last Login</th></tr></thead>
        <tbody>
          <tr *ngFor="let l of analytics()!.recentLogins">
            <td>{{l.email}}</td>
            <td><span class="rb role-{{l.role.toLowerCase()}}">{{l.role}}</span></td>
            <td class="sec">{{l.lastLoginAt|date:'MMM d, HH:mm'}}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!-- TAB: NOTIFY -->
  <div *ngIf="tab==='notify'" class="notify-form">
    <div class="section-title">📢 Send Platform Notification to All Active Users</div>
    <input id="notif-title" class="inp" placeholder="Notification title..." [(ngModel)]="notifTitle">
    <textarea id="notif-msg" class="textarea" placeholder="Message body..." [(ngModel)]="notifMsg" rows="4"></textarea>
    <button id="btn-send-notif" class="btn-send" (click)="sendNotif()" [disabled]="sending()">
      {{sending()?'Sending...':'📤 Send to All Users'}}
    </button>
  </div>

  <!-- TAB: AUDIT LOGS -->
  <div *ngIf="tab==='audit'">
    <button class="btn-refresh" (click)="loadAudit()">↻ Refresh</button>
    <div class="loading" *ngIf="loadingAudit()">Loading audit logs...</div>
    <table class="tbl" *ngIf="!loadingAudit()">
      <thead><tr><th>Time</th><th>Action</th><th>Detail</th><th>Actor</th></tr></thead>
      <tbody>
        <tr *ngFor="let a of auditLogs()">
          <td class="sec">{{a.timestamp|date:'MMM d HH:mm:ss'}}</td>
          <td><span class="rb">{{a.action}}</span></td>
          <td>{{a.detail}}</td>
          <td class="sec">{{a.actor}}</td>
        </tr>
        <tr *ngIf="auditLogs().length===0"><td colspan="4" class="empty">No audit events yet.</td></tr>
      </tbody>
    </table>
  </div>

  <!-- TAB: REPORT -->
  <div *ngIf="tab==='report'">
    <button id="btn-gen-report" class="btn-send" (click)="loadReport()">📊 Generate Activity Report</button>
    <div class="loading" *ngIf="loadingRep()">Generating...</div>
    <div *ngIf="report() && !loadingRep()" class="report-box">
      <div class="report-meta">
        <span>Generated by <b>{{report()!.generatedBy}}</b></span>
        <span>at {{report()!.generatedAt|date:'medium'}}</span>
      </div>
      <div class="stats">
        <div class="stat"><b>{{report()!.totalUsers}}</b><span>Total</span></div>
        <div class="stat g"><b>{{report()!.activeUsers}}</b><span>Active</span></div>
        <div class="stat r"><b>{{report()!.suspendedUsers}}</b><span>Suspended</span></div>
      </div>
      <div class="section-title" style="margin-top:12px">Recent Signups (last 20)</div>
      <table class="tbl">
        <thead><tr><th>Email</th><th>Role</th><th>Joined</th><th>Active</th></tr></thead>
        <tbody>
          <tr *ngFor="let s of report()!.recentSignups">
            <td>{{s.email}}</td>
            <td><span class="rb role-{{(s.role||'member').toLowerCase()}}">{{s.role}}</span></td>
            <td class="sec">{{s.createdAt|date:'MMM d, y'}}</td>
            <td><span class="sb" [class.active]="s.isActive" [class.sus]="!s.isActive">{{s.isActive?'Yes':'No'}}</span></td>
          </tr>
        </tbody>
      </table>
      <p class="sec" style="margin-top:8px;font-size:12px">{{report()!.auditSummary}}</p>
    </div>
  </div>
</div>
  `,
  styles: [`
.ap{background:var(--color-surface);border:1px solid var(--color-border);border-radius:12px;padding:24px;margin:20px 0}
.ap-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:18px}
.ap-title{display:flex;align-items:center;gap:12px}
.ap-title span{font-size:28px}
.ap-title h2{font-size:18px;font-weight:700;margin:0 0 2px}
.ap-title p{font-size:12px;color:var(--color-text-secondary);margin:0}
.btn-x{padding:6px 12px;border:1px solid var(--color-border);border-radius:6px;color:var(--color-text-secondary);background:transparent;cursor:pointer;font-size:12px}
.btn-x:hover{color:#ef4444;border-color:#ef4444}
.stats{display:flex;gap:10px;flex-wrap:wrap;margin-bottom:16px}
.stat{background:var(--color-bg);border:1px solid var(--color-border);border-radius:8px;padding:12px 16px;text-align:center;min-width:80px}
.stat b{display:block;font-size:22px;font-weight:700}
.stat span{font-size:10px;color:var(--color-text-secondary);text-transform:uppercase}
.stat.g b{color:#22c55e}.stat.r b{color:#ef4444}.stat.b b{color:#3b82f6}.stat.p b{color:#a855f7}.stat.o b{color:#f59e0b}
.tabs{display:flex;gap:4px;margin-bottom:16px;flex-wrap:wrap}
.tabs button{padding:7px 14px;border:1px solid var(--color-border);border-radius:6px;background:transparent;color:var(--color-text-secondary);cursor:pointer;font-size:12px;font-weight:500;transition:all .15s}
.tabs button.active{background:var(--color-accent);color:white;border-color:var(--color-accent)}
.toast{padding:9px 14px;border-radius:6px;font-size:12px;margin-bottom:10px}
.toast.success{background:rgba(34,197,94,.15);color:#22c55e}.toast.error{background:rgba(239,68,68,.12);color:#ef4444}
.filter-row{display:flex;gap:8px;margin-bottom:12px;flex-wrap:wrap}
.inp{padding:8px 12px;background:var(--color-bg);border:1px solid var(--color-border);border-radius:6px;color:var(--color-text);font-size:13px;flex:1;min-width:160px}
.sel{padding:8px 10px;background:var(--color-bg);border:1px solid var(--color-border);border-radius:6px;color:var(--color-text);font-size:12px}
.tbl{width:100%;border-collapse:collapse;font-size:12px}
.tbl th{text-align:left;padding:8px 10px;border-bottom:1px solid var(--color-border);color:var(--color-text-secondary);font-size:11px;font-weight:600;text-transform:uppercase}
.tbl td{padding:10px 10px;border-bottom:1px solid var(--color-border);color:var(--color-text);vertical-align:middle}
.dim td{opacity:.5}
.uc{display:flex;align-items:center;gap:8px;font-weight:500}
.sec{color:var(--color-text-secondary)}
.av{width:28px;height:28px;border-radius:50%;background:linear-gradient(135deg,#6366f1,#8b5cf6);display:flex;align-items:center;justify-content:center;font-size:10px;font-weight:700;color:white;flex-shrink:0}
.rb{padding:2px 8px;border-radius:12px;font-size:10px;font-weight:600}
.role-member{background:rgba(59,130,246,.15);color:#3b82f6}
.role-boardadmin{background:rgba(168,85,247,.15);color:#a855f7}
.role-platformadmin{background:rgba(245,158,11,.15);color:#f59e0b}
.sb{padding:2px 8px;border-radius:12px;font-size:10px;font-weight:600}
.sb.active{background:rgba(34,197,94,.15);color:#22c55e}
.sb.sus{background:rgba(239,68,68,.12);color:#ef4444}
.ac{display:flex;align-items:center;gap:5px}
.rsel{padding:4px 6px;background:var(--color-bg);border:1px solid var(--color-border);border-radius:4px;color:var(--color-text);font-size:11px}
.ba{padding:3px 8px;border-radius:4px;font-size:11px;cursor:pointer;font-weight:600}
.sus-btn{background:rgba(234,179,8,.15);color:#eab308;border:1px solid rgba(234,179,8,.3)}
.act-btn{background:rgba(34,197,94,.15);color:#22c55e;border:1px solid rgba(34,197,94,.3)}
.del-btn{background:rgba(239,68,68,.1);color:#ef4444;border:1px solid rgba(239,68,68,.2)}
.empty{text-align:center;color:var(--color-text-secondary);padding:20px}
.loading{color:var(--color-text-secondary);font-size:13px;padding:16px 0}
.btn-refresh{padding:7px 14px;border:1px solid var(--color-border);border-radius:6px;background:transparent;color:var(--color-text-secondary);cursor:pointer;font-size:12px;margin-bottom:12px}
.section-title{font-size:11px;font-weight:600;text-transform:uppercase;letter-spacing:.5px;color:var(--color-text-secondary);margin-bottom:8px}
.dist-row{display:flex;gap:10px;flex-wrap:wrap}
.dist-card{background:var(--color-bg);border:1px solid var(--color-border);border-radius:8px;padding:12px 20px;text-align:center}
.dist-card b{display:block;font-size:22px;font-weight:700}
.dist-card span{font-size:11px;color:var(--color-text-secondary)}
.notify-form{display:flex;flex-direction:column;gap:10px;max-width:560px}
.textarea{padding:10px 12px;background:var(--color-bg);border:1px solid var(--color-border);border-radius:6px;color:var(--color-text);font-size:13px;resize:vertical}
.btn-send{padding:10px 20px;background:var(--color-accent);color:white;border:none;border-radius:6px;font-size:13px;font-weight:600;cursor:pointer}
.btn-send:disabled{opacity:.5;cursor:not-allowed}
.report-box{margin-top:14px}
.report-meta{display:flex;gap:16px;font-size:12px;color:var(--color-text-secondary);margin-bottom:12px}
  `]
})
export class AdminPanelComponent implements OnInit {
  private svc = inject(AdminService);
  @Output() onClose = new EventEmitter<void>();

  tab: Tab = 'users';
  tabList = [
    {key:'users' as Tab, label:'👥 Users'},
    {key:'workspaces' as Tab, label:'🏢 Workspaces'},
    {key:'boards' as Tab, label:'📋 Boards'},
    {key:'analytics' as Tab, label:'📊 Analytics'},
    {key:'notify' as Tab, label:'📢 Notify'},
    {key:'audit' as Tab, label:'📜 Audit Logs'},
    {key:'report' as Tab, label:'📁 Report'},
  ];

  // Users
  users = signal<UserDto[]>([]); stats = signal<AdminDashboardDto|null>(null);
  loadingUsers = signal(true); uq=''; uRole=''; uStatus='';

  // Workspaces
  workspaces = signal<any[]>([]); loadingWS = signal(false);

  // Boards
  boards = signal<any[]>([]); loadingBoards = signal(false);

  // Analytics
  analytics = signal<PlatformAnalyticsDto|null>(null); loadingAn = signal(false);

  // Notify
  notifTitle=''; notifMsg=''; sending = signal(false);

  // Audit
  auditLogs = signal<AuditLogDto[]>([]); loadingAudit = signal(false);

  // Report
  report = signal<ActivityReportDto|null>(null); loadingRep = signal(false);

  // Toast
  toast = signal(''); toastType = signal<'success'|'error'>('success');

  ngOnInit() { this.svc.getDashboard().subscribe({next:r=>{if(r.data)this.stats.set(r.data)}});this.loadUsers(); }

  switchTab(t: Tab) {
    this.tab = t;
    if(t==='workspaces'&&this.workspaces().length===0)this.loadWorkspaces();
    if(t==='boards'&&this.boards().length===0)this.loadBoards();
    if(t==='analytics'&&!this.analytics())this.loadAnalytics();
    if(t==='audit')this.loadAudit();
  }

  loadUsers() { this.loadingUsers.set(true); this.svc.getAllUsers().subscribe({next:r=>{this.users.set(r.data??[]);this.loadingUsers.set(false)},error:()=>this.loadingUsers.set(false)}); }

  filteredUsers() {
    const q=this.uq.toLowerCase();
    return this.users().filter(u=>{
      const mq=!q||u.fullName.toLowerCase().includes(q)||u.email.toLowerCase().includes(q);
      const mr=!this.uRole||u.role===this.uRole;
      const ms=!this.uStatus||(this.uStatus==='active'?u.isActive:!u.isActive);
      return mq&&mr&&ms;
    });
  }

  isPrimary(u:UserDto){return u.userId==='00000000-0000-0000-0000-000000000001';}
  ini(n:string){return(n??'?').split(' ').map((p:string)=>p[0]).join('').toUpperCase().slice(0,2);}

  chRole(u:UserDto,r:string){if(r===u.role)return;this.svc.changeRole(u.userId,r).subscribe({next:res=>{if(res.data){this.users.update(l=>l.map(x=>x.userId===u.userId?{...x,role:res.data!.role}:x));this.showToast(`Role → ${r}`,'success');}},error:e=>this.showToast(e.message,'error')});}
  sus(u:UserDto){if(!confirm(`Suspend ${u.fullName}?`))return;this.svc.suspendUser(u.userId).subscribe({next:r=>{if(r.data){this.users.update(l=>l.map(x=>x.userId===u.userId?{...x,isActive:false}:x));this.showToast('Suspended','success');}},error:e=>this.showToast(e.message,'error')});}
  act(u:UserDto){this.svc.reactivateUser(u.userId).subscribe({next:r=>{if(r.data){this.users.update(l=>l.map(x=>x.userId===u.userId?{...x,isActive:true}:x));this.showToast('Reactivated','success');}},error:e=>this.showToast(e.message,'error')});}
  del(u:UserDto){if(!confirm(`Delete ${u.fullName} permanently?`))return;this.svc.deleteUser(u.userId).subscribe({next:()=>{this.users.update(l=>l.filter(x=>x.userId!==u.userId));this.showToast('Deleted','success');},error:e=>this.showToast(e.message,'error')});}

  loadWorkspaces(){this.loadingWS.set(true);this.svc.getWorkspaces().subscribe({next:r=>{const d=r?.data??r;this.workspaces.set(Array.isArray(d)?d:[]);this.loadingWS.set(false);},error:()=>{this.workspaces.set([]);this.loadingWS.set(false);}});}
  delWS(w:any){if(!confirm(`Delete workspace "${w.name}"?`))return;this.svc.deleteWorkspace(w.workspaceId??w.id).subscribe({next:()=>{this.workspaces.update(l=>l.filter(x=>x!==w));this.showToast('Workspace deleted','success');},error:e=>this.showToast(e.message,'error')});}

  loadBoards(){this.loadingBoards.set(true);this.svc.getBoards().subscribe({next:r=>{const d=r?.data??r;this.boards.set(Array.isArray(d)?d:[]);this.loadingBoards.set(false);},error:()=>{this.boards.set([]);this.loadingBoards.set(false);}});}
  delBoard(b:any){if(!confirm(`Delete board "${b.name}"?`))return;this.svc.deleteBoard(b.boardId??b.id).subscribe({next:()=>{this.boards.update(l=>l.filter(x=>x!==b));this.showToast('Board deleted','success');},error:e=>this.showToast(e.message,'error')});}

  loadAnalytics(){this.loadingAn.set(true);this.svc.getAnalytics().subscribe({next:r=>{if(r.data)this.analytics.set(r.data);this.loadingAn.set(false);},error:()=>this.loadingAn.set(false)});}
  roleEntries():Array<[string,number]>{const d=this.analytics()?.roleDistribution??{};return Object.entries(d) as [string,number][];}

  sendNotif(){if(!this.notifTitle||!this.notifMsg)return;this.sending.set(true);this.svc.sendPlatformNotification(this.notifTitle,this.notifMsg).subscribe({next:r=>{this.sending.set(false);this.showToast(r.message??'Sent!','success');this.notifTitle='';this.notifMsg='';},error:e=>{this.sending.set(false);this.showToast(e.message,'error');}});}

  loadAudit(){this.loadingAudit.set(true);this.svc.getAuditLogs().subscribe({next:r=>{this.auditLogs.set(r.data??[]);this.loadingAudit.set(false);},error:()=>this.loadingAudit.set(false)});}

  loadReport(){this.loadingRep.set(true);this.svc.generateReport().subscribe({next:r=>{if(r.data)this.report.set(r.data);this.loadingRep.set(false);},error:()=>this.loadingRep.set(false)});}

  private showToast(m:string,t:'success'|'error'){this.toast.set(m);this.toastType.set(t);setTimeout(()=>this.toast.set(''),3000);}
}
