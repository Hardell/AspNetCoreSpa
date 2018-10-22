import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { SharedModule } from '@app/shared';
import { SignalrComponent } from './signalr.component';
import { ChatComponent } from './chat/chat.component';

import { routes } from './signalr.routes';

@NgModule({
    imports: [
        SharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        SignalrComponent,
        ChatComponent
    ]
})
export class SignalrModule { }
