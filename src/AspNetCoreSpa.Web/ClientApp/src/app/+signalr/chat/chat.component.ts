import { Component, OnInit, Inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';

@Component({
  selector: 'appc-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit {

  private _hubConnection: HubConnection;
  public async: any;
  message = '';
  messages: string[] = [];

  constructor(@Inject('BASE_URL') private baseUrl: string) { }

  public sendMessage(): void {
    this._hubConnection.invoke('send', this.message);
    // this.messages.push(this.message);
    this.message = '';
  }

  ngOnInit() {
    const token = localStorage.getItem('access_token');
    this._hubConnection = new HubConnectionBuilder().withUrl(`${this.baseUrl}chathub?access_token=${token}`).build();

    this._hubConnection.on('send', (data: any) => {
    // const received = `${data}`;
    this.messages.push(data);
    });

    this._hubConnection.start()
    .then(() => {
    console.log('Hub connection started');
    })
    .catch(err => {
    console.log('Error while establishing connection: ' + err);
    });
  }

}
