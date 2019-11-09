import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { Observable } from 'rxjs';
import { Edge } from '../models/edge.model';

@Injectable({
  providedIn: 'root'
})
export class GraphService {
  private readonly baseUrl = 'https://localhost:5001/';
  private readonly graphApiUrl = this.baseUrl + 'graph';
  private readonly graphHubUrl = this.baseUrl + 'graphhub';
  private connectionIsEstablished = false;  
  private _hubConnection: HubConnection;

  messageReceived = new EventEmitter<Edge[]>();  
  connectionEstablished = new EventEmitter<Boolean>();

  constructor(private http: HttpClient) { 
    // this.createConnection();  
    // this.registerOnServerEvents();  
    // this.startConnection(); 
  }

  getEdges(numWords = 10, batchSize = 2): Observable<Edge[]> {
    return this.http.get<Edge[]>(this.graphApiUrl + '?numWords=' + numWords.toString() + '&batchSize=' + batchSize.toString());
  }


  sendMessage(numWords: number, batchSize: number) {  
    this._hubConnection.invoke('NewMessage', numWords, batchSize);  
  }  
  
  private createConnection() {  
    this._hubConnection = new HubConnectionBuilder()  
      .withUrl(this.graphHubUrl)
      //.AddNewtonsoftJsonProtocol()  
      .build();  
  }  
  
  private startConnection(): void {  
    this._hubConnection  
      .start()  
      .then(() => {  
        this.connectionIsEstablished = true;  
        console.log('Hub connection started');  
        this.connectionEstablished.emit(true);  
        this.sendMessage(10, 2);
      })  
      .catch(err => {  
        console.log('Error while establishing connection, retrying...');  
        setTimeout(function () { this.startConnection(); }, 5000);  
      });  
  }  
  
  private registerOnServerEvents(): void {  
    this._hubConnection.on('MessageReceived', (data: Edge[]) => {  
      this.messageReceived.emit(data);  
    });  
  }  
}
