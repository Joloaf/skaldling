import { apiGet, apiPost, apiPut, type ApiResult } from './client';

// POST /api/heroes
export type CreateHeroCommand = {
	name: string;
	avatarSpriteIds: string[];
};

export type CreateHeroResponse = {
	id: string;
	name: string;
	createdAt: string; // JSON has no Date type, using string instead on serialize
	avatarSpriteIds: string[];
};

// GET /api/heroes/{id}
export type HeroDto = {
	id: string;
	name: string;
	achievementPoints: number;
	createdAt: string;
	updatedAt: string;
	avatarSpriteIds: string[];
};

// GET ALL /api/heroes (Reusing HeroDto, will split into HeroSummaryDto once list/detail shapes aren't identical)
export type ListHeroesResponse = {
	heroes: HeroDto[];
}

// PUT /api/heroes/{id}/avatar
export type UpdateAvatarCommand = {
	avatarSpriteIds: string[];
}

export type UpdateAvatarResponse = {
	heroId: string;
	avatarSpriteIds: string[];
	updatedAt: string;
}

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
	}
};
