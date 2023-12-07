export interface IColleagueModel {
  userId: number | undefined;
  colleagueId: number | undefined;
  colleague:
    | {
        id: number | undefined;
        username: string | undefined;
        email: string | undefined;
      }
    | undefined;
}
