import { apiGet, apiPost, apiPut, type ApiResult } from './client';

// POST /api/heroes
export type CreateHeroCommand = {
	name: string;
	spriteIds: string[];
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

// PUT /api/heroes/{id}/avatar
export type UpdateAvatarCommand = {
	spriteIds: string[];
}

export type UpdateAvatarResponse = {
	heroId: string;
	spriteIds: string[];
	updatedAt: string;
}

export const heroesApi = {
	create(command: CreateHeroCommand): Promise<ApiResult<CreateHeroResponse>> {
		return apiPost<CreateHeroResponse, CreateHeroCommand>('/api/heroes', command);
	},
	get(id: string): Promise<ApiResult<HeroDto>> {
		return apiGet<HeroDto>(`/api/heroes/${id}`);
	},
	updateAvatar(id: string, command: UpdateAvatarCommand): Promise<ApiResult<UpdateAvatarResponse>> {
		return apiPut<UpdateAvatarResponse, UpdateAvatarCommand>(`/api/heroes/${id}/avatar`, command);
	}
};
