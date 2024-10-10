import { User } from "./user";

export class LikeParams{
    pageSize: number = 5;
    pageNumber: number = 1;
    userId: string = '';

    constructor(user: User | null) { 
        this.userId = user.;     
    }
}