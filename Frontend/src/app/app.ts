import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './layout/navbar/navbar.component';
import { ToastComponent } from './shared/toast/toast.component';

@Component({
    selector: 'app-root',
    imports: [RouterOutlet, NavbarComponent, ToastComponent],
    templateUrl: './app.html',
    styleUrl: './app.scss'
})
export class App { }
