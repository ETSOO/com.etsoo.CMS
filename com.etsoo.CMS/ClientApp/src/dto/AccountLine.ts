export type AccountLine = {
  id: number;
  kind: string;
  isCollection: boolean;
  subject: string;
  subjectId: number;
  title: string;
  amount: number;
  entityStatus: number;
  creation: Date;
  estimatedDate?: Date;
  happenDate?: Date;
  repeat?: number;
  reference?: string;
  companyAccount?: number;
  externalAccount?: number;
  selfLine?: boolean;
  user: string;
};
