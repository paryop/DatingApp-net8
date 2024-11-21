import { ResolveFn } from '@angular/router';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';
import { inject } from '@angular/core';

export const MemberDetailResolver: ResolveFn<Member | null> = (route, state) => {
  const memberService = inject(MembersService);
  const userName = route.paramMap.get("username");

  if(!userName) return null
  return memberService.getMember(userName);
};
