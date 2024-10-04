import { User } from "./user";

export class UserParams{
    gender: string;
    minAge: number = 18;
    maxAge: number = 99;
    pageSize: number = 5;
    pageNumber: number = 1;
    orderBy: string = "lastActive";

    constructor(user: User | null) { 
        this.gender = user?.gender == 'female' ? 'male' : 'female';     
    }
}