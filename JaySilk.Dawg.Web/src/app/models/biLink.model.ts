import { Vertex } from './vertex.model';

export interface BiLink {
    source: Vertex;
    intermediate: Vertex;
    target: Vertex;
    key: string;
    linknum: number;
}