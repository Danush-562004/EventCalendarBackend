export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
    errors?: string[];
}

export interface PagedResponse<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface CategoryResponse {
    id: number;
    name: string;
    description?: string;
    colorCode: string;
    isActive: boolean;
    createdAt: string;
}

export interface VenueResponse {
    id: number;
    name: string;
    address: string;
    city: string;
    state: string;
    country: string;
    zipCode?: string;
    capacity: number;
    description?: string;
    contactEmail?: string;
    contactPhone?: string;
    isActive: boolean;
    createdAt: string;
}

export interface EventResponse {
    id: number;
    title: string;
    description?: string;
    startDateTime: string;
    endDateTime: string;
    location?: string;
    privacy: string;
    isAllDay: boolean;
    reminderEnabled: boolean;
    reminderMinutesBefore?: number;
    recurrence: string;
    maxAttendees: number;
    ticketCount: number;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
    userId: number;
    organizerName: string;
    category: CategoryResponse;
    venue?: VenueResponse;
}

export interface ReminderResponse {
    id: number;
    title: string;
    message?: string;
    reminderDateTime: string;
    type: string;
    isSent: boolean;
    sentAt?: string;
    isActive: boolean;
    createdAt: string;
    eventId: number;
    eventTitle: string;
    userId: number;
}

export interface TicketResponse {
    id: number;
    ticketNumber: string;
    type: string;
    status: string;
    price: number;
    quantity: number;
    seatNumber?: string;
    checkedIn: boolean;
    createdAt: string;
    eventId: number;
    eventTitle: string;
    userId: number;
    userFullName: string;
}
