import { create } from 'zustand';
import { combine } from 'zustand/middleware';

export interface Notify {
  title: string;
  desc: string;
}

const useUserStore = create(
  combine(
    {
      name: '',
    },
    (set) => ({
      setName: (newName: string) =>
        set(() => ({
          name: newName,
        })),
    })
  )
);

export default useUserStore;
