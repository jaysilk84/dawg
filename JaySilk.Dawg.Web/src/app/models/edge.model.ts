import { Vertex } from './vertex.model';

export interface Edge {
    source: Vertex;
    target: Vertex; 
    key: string;
    linknum: number;
}