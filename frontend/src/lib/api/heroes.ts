import { apiGet, apiPost, apiPut, type ApiResult } from './client';

// POST /api/heroes
export type CreateHeroCommand = {
	name: string;
	readingAge: number;
	avatarSpriteIds: string[];
};

export type CreateHeroResponse = {
	id: string;
	name: string;
	readingAge: number;
	createdAt: string; // JSON has no Date type, using string instead on serialize
	avatarSpriteIds: string[];
};

export type PastAdventureSummary = {
	id: string;
	title: string;
	completedAt: string;
}

// GET /api/heroes/{id}
export type HeroDto = {
	id: string;
	name: string;
	readingAge: number;
	achievementPoints: number;
	createdAt: string;
	updatedAt: string;
	avatarSpriteIds: string[];
	activeAdventureId?: string;
	activeAdventureTitle?: string;
	pastAdventures: PastAdventureSummary[];
};

// GET /api/heroes
export type HeroSummaryDto = {
	id: string;
	name: string;
	readingAge: number;
	achievementPoints: number;
	createdAt: string;
	updatedAt: string;
	avatarSpriteIds: string[];
};

export type ListHeroesResponse = {
	heroes: HeroSummaryDto[];
};

// PUT /api/heroes/{id}/avatar
export type UpdateAvatarCommand = {
	avatarSpriteIds: string[];
};

export type UpdateAvatarResponse = {
	heroId: string;
	avatarSpriteIds: string[];
	updatedAt: string;
};

// PUT /api/heroes/{id}/details
export type UpdateHeroDetailsCommand = {
	name: string;
	readingAge: number;
};

export type UpdateHeroDetailsResponse = {
	heroId: string;
	name: string;
	readingAge: number;
	updatedAt: string;
};

export const heroesApi = {
	create(command: CreateHeroCommand): Promise<ApiResult<CreateHeroResponse>> {
		return apiPost<CreateHeroResponse, CreateHeroCommand>('/api/heroes', command);
	},
	get(id: string): Promise<ApiResult<HeroDto>> {
		return apiGet<HeroDto>(`/api/heroes/${id}`);
	},
	list(): Promise<ApiResult<ListHeroesResponse>> {
		return apiGet<ListHeroesResponse>('/api/heroes');
	},
	updateAvatar(id: string, command: UpdateAvatarCommand): Promise<ApiResult<UpdateAvatarResponse>> {
		return apiPut<UpdateAvatarResponse, UpdateAvatarCommand>(`/api/heroes/${id}/avatar`, command);
	},
	updateDetails(id: string, command: UpdateHeroDetailsCommand): Promise<ApiResult<UpdateHeroDetailsResponse>> {
		return apiPut<UpdateHeroDetailsResponse, UpdateHeroDetailsCommand>(`/api/heroes/${id}/details`, command);
	}
};
