import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { SharedModule } from '../shared/shared.module';
import { SignalrComponent } from './signalr.component';
import { ChatComponent } from './chat/chat.component';

@NgModule({
    imports: [
        SharedModule,
        RouterModule.forChild([
            {
                path: '', component: SignalrComponent,
                children: [
                    { path: '', redirectTo: 'chat' },
                    { path: 'chat', component: ChatComponent, data: { state: 'chat' } }
                ]
            }
        ])
    ],
    declarations: [
        SignalrComponent,
        ChatComponent
    ]
})
export class SignalrModule { }
