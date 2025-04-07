export interface Contact {
  id: number;
  firstName: string;
  lastName: string;
  email?: string | null; // Use optional or null
  phone?: string | null;
}