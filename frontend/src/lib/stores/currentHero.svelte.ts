import { browser } from '$app/environment';

const STORAGE_KEY = 'skaldling:currentHeroId';

let heroId = $state<string | null>(browser ? localStorage.getItem(STORAGE_KEY) : null);

export const currentHero = {
	get id(): string | null {
		return heroId;
	},

	set(id: string): void {
		heroId = id;
		if (browser) localStorage.setItem(STORAGE_KEY, id);
	},

	clear(): void {
		heroId = null;
		if (browser) localStorage.removeItem(STORAGE_KEY);
	}
};
