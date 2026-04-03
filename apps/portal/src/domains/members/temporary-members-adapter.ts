import { PortalRole } from '@/platform/auth/types';

export type MemberRecord = {
  id: string;
  name: string;
  email: string;
  role: PortalRole;
  status: 'active' | 'pending';
  isCurrentUser?: boolean;
};

export type InviteMemberInput = {
  name: string;
  email: string;
  role: PortalRole;
};

const STORAGE_PREFIX = 'basefaq.portal.temporary-members';

function storageKey(tenantId: string) {
  return `${STORAGE_PREFIX}.${tenantId}`;
}

function read(tenantId: string) {
  const raw = window.localStorage.getItem(storageKey(tenantId));
  if (!raw) {
    return [] as MemberRecord[];
  }

  try {
    return JSON.parse(raw) as MemberRecord[];
  } catch {
    return [] as MemberRecord[];
  }
}

function write(tenantId: string, members: MemberRecord[]) {
  window.localStorage.setItem(storageKey(tenantId), JSON.stringify(members));
}

// TODO: Replace this adapter with the real Portal members API when it exists.
export function listTemporaryMembers(
  tenantId: string,
  currentUser?: { name?: string; email?: string; role?: PortalRole },
) {
  const stored = read(tenantId);
  if (stored.length > 0) {
    return stored;
  }

  const seed: MemberRecord[] = [
    {
      id: crypto.randomUUID(),
      name: currentUser?.name ?? 'Current user',
      email: currentUser?.email ?? 'owner@basefaq.local',
      role: currentUser?.role ?? 'Admin',
      status: 'active',
      isCurrentUser: true,
    },
    {
      id: crypto.randomUUID(),
      name: 'Pending teammate',
      email: 'invite@basefaq.local',
      role: 'Member',
      status: 'pending',
    },
  ];

  write(tenantId, seed);
  return seed;
}

export function inviteTemporaryMember(
  tenantId: string,
  input: InviteMemberInput,
) {
  const members = read(tenantId);
  const nextMembers = [
    ...members,
    {
      id: crypto.randomUUID(),
      name: input.name,
      email: input.email,
      role: input.role,
      status: 'pending' as const,
    },
  ];

  write(tenantId, nextMembers);
  return nextMembers;
}

export function removeTemporaryMember(tenantId: string, memberId: string) {
  const nextMembers = read(tenantId).filter((member) => member.id !== memberId);
  write(tenantId, nextMembers);
  return nextMembers;
}
