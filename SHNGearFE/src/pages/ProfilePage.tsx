import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapPin, Pencil, Plus, Save, Star, Trash2, User } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { accountApi } from '../api/account';
import { addressApi } from '../api/address';
import type { AddressDto, CreateAddressRequest, UpdateAddressRequest } from '../types';

interface AddressFormState {
  recipientName: string;
  phoneNumber: string;
  province: string;
  district: string;
  ward: string;
  street: string;
  note: string;
  isDefault: boolean;
}

const emptyAddressForm: AddressFormState = {
  recipientName: '',
  phoneNumber: '',
  province: '',
  district: '',
  ward: '',
  street: '',
  note: '',
  isDefault: false,
};

export default function ProfilePage() {
  const navigate = useNavigate();
  const { user, isAuthenticated, refreshCurrentUser } = useAuth();

  const [profileForm, setProfileForm] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
    address: '',
  });
  const [savingProfile, setSavingProfile] = useState(false);
  const [profileMessage, setProfileMessage] = useState<string | null>(null);

  const [addresses, setAddresses] = useState<AddressDto[]>([]);
  const [addressForm, setAddressForm] = useState<AddressFormState>(emptyAddressForm);
  const [editingAddressId, setEditingAddressId] = useState<string | null>(null);
  const [loadingAddresses, setLoadingAddresses] = useState(true);
  const [savingAddress, setSavingAddress] = useState(false);
  const [addressMessage, setAddressMessage] = useState<string | null>(null);

  const addressFormValid = useMemo(() => {
    return (
      addressForm.recipientName.trim() &&
      addressForm.phoneNumber.trim() &&
      addressForm.province.trim() &&
      addressForm.district.trim() &&
      addressForm.ward.trim() &&
      addressForm.street.trim()
    );
  }, [addressForm]);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    setProfileForm({
      firstName: user?.firstName || '',
      lastName: user?.lastName || '',
      phoneNumber: user?.phoneNumber || '',
      address: user?.address || '',
    });

    void fetchAddresses();
  }, [isAuthenticated, navigate, user?.address, user?.firstName, user?.lastName, user?.phoneNumber]);

  const fetchAddresses = async () => {
    setLoadingAddresses(true);
    setAddressMessage(null);
    try {
      const res = await addressApi.getMyAddresses();
      if (res.data.success) {
        setAddresses(res.data.data);
      }
    } catch {
      setAddressMessage('Could not load your saved addresses.');
    } finally {
      setLoadingAddresses(false);
    }
  };

  const handleSaveProfile = async (event: React.FormEvent) => {
    event.preventDefault();
    setSavingProfile(true);
    setProfileMessage(null);

    try {
      const payload = {
        firstName: profileForm.firstName.trim() || undefined,
        lastName: profileForm.lastName.trim() || undefined,
        phoneNumber: profileForm.phoneNumber.trim() || undefined,
        address: profileForm.address.trim() || undefined,
      };

      const res = await accountApi.updateMyProfile(payload);
      if (res.data.success) {
        await refreshCurrentUser();
        setProfileMessage('Profile updated successfully.');
      }
    } catch {
      setProfileMessage('Failed to update profile. Please try again.');
    } finally {
      setSavingProfile(false);
    }
  };

  const startCreateAddress = () => {
    setEditingAddressId(null);
    setAddressForm(emptyAddressForm);
    setAddressMessage(null);
  };

  const startEditAddress = (address: AddressDto) => {
    setEditingAddressId(address.id);
    setAddressForm({
      recipientName: address.recipientName,
      phoneNumber: address.phoneNumber,
      province: address.province,
      district: address.district,
      ward: address.ward,
      street: address.street,
      note: address.note || '',
      isDefault: address.isDefault,
    });
    setAddressMessage(null);
  };

  const handleSaveAddress = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!addressFormValid || savingAddress) {
      return;
    }

    setSavingAddress(true);
    setAddressMessage(null);

    const payload: CreateAddressRequest | UpdateAddressRequest = {
      recipientName: addressForm.recipientName.trim(),
      phoneNumber: addressForm.phoneNumber.trim(),
      province: addressForm.province.trim(),
      district: addressForm.district.trim(),
      ward: addressForm.ward.trim(),
      street: addressForm.street.trim(),
      note: addressForm.note.trim() || undefined,
      isDefault: addressForm.isDefault,
    };

    try {
      if (editingAddressId) {
        await addressApi.update(editingAddressId, payload);
      } else {
        await addressApi.create(payload);
      }

      await fetchAddresses();
      setEditingAddressId(null);
      setAddressForm(emptyAddressForm);
      setAddressMessage('Address saved successfully.');
    } catch {
      setAddressMessage('Could not save address. Please check your data.');
    } finally {
      setSavingAddress(false);
    }
  };

  const handleDeleteAddress = async (id: string) => {
    const confirmDelete = window.confirm('Delete this address?');
    if (!confirmDelete) {
      return;
    }

    try {
      await addressApi.delete(id);
      await fetchAddresses();
      if (editingAddressId === id) {
        setEditingAddressId(null);
        setAddressForm(emptyAddressForm);
      }
      setAddressMessage('Address deleted.');
    } catch {
      setAddressMessage('Failed to delete address.');
    }
  };

  const handleSetDefault = async (id: string) => {
    try {
      await addressApi.setDefault(id);
      await fetchAddresses();
      setAddressMessage('Default address updated.');
    } catch {
      setAddressMessage('Failed to set default address.');
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">My Profile</h1>
        <p className="text-gray-500 mt-1">Update your account details and delivery addresses.</p>
      </div>

      <div className="grid gap-8 lg:grid-cols-5">
        <section className="lg:col-span-2 bg-white border border-gray-200 rounded-2xl p-6 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <User className="w-5 h-5 text-gray-700" />
            <h2 className="text-xl font-semibold text-gray-900">Account Information</h2>
          </div>

          <form onSubmit={handleSaveProfile} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                value={user?.email || ''}
                disabled
                className="w-full px-3 py-2 rounded-lg border border-gray-200 bg-gray-50 text-gray-500"
              />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">First Name</label>
                <input
                  value={profileForm.firstName}
                  onChange={(e) => setProfileForm((prev) => ({ ...prev, firstName: e.target.value }))}
                  className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
                <input
                  value={profileForm.lastName}
                  onChange={(e) => setProfileForm((prev) => ({ ...prev, lastName: e.target.value }))}
                  className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Phone Number</label>
              <input
                value={profileForm.phoneNumber}
                onChange={(e) => setProfileForm((prev) => ({ ...prev, phoneNumber: e.target.value }))}
                className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Address Note</label>
              <textarea
                value={profileForm.address}
                onChange={(e) => setProfileForm((prev) => ({ ...prev, address: e.target.value }))}
                rows={3}
                className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
              />
            </div>

            {profileMessage && (
              <p className="text-sm text-gray-600">{profileMessage}</p>
            )}

            <button
              type="submit"
              disabled={savingProfile}
              className="inline-flex items-center gap-2 gradient-btn text-white px-4 py-2.5 rounded-lg text-sm font-semibold disabled:opacity-60"
            >
              <Save className="w-4 h-4" />
              {savingProfile ? 'Saving...' : 'Save Profile'}
            </button>
          </form>
        </section>

        <section className="lg:col-span-3 space-y-4">
          <div className="bg-white border border-gray-200 rounded-2xl p-6 shadow-sm">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <MapPin className="w-5 h-5 text-gray-700" />
                <h2 className="text-xl font-semibold text-gray-900">Delivery Addresses</h2>
              </div>
              <button
                onClick={startCreateAddress}
                className="inline-flex items-center gap-2 px-3 py-2 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                <Plus className="w-4 h-4" />
                New Address
              </button>
            </div>

            {loadingAddresses ? (
              <p className="text-sm text-gray-500">Loading addresses...</p>
            ) : addresses.length === 0 ? (
              <p className="text-sm text-gray-500">No address yet. Add one to speed up checkout.</p>
            ) : (
              <div className="space-y-3">
                {addresses.map((address) => (
                  <article key={address.id} className="border border-gray-200 rounded-xl p-4">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="font-semibold text-gray-900">{address.recipientName}</p>
                        <p className="text-sm text-gray-600">{address.phoneNumber}</p>
                        <p className="text-sm text-gray-700 mt-1">
                          {address.street}, {address.ward}, {address.district}, {address.province}
                        </p>
                        {address.note && <p className="text-xs text-gray-500 mt-1">Note: {address.note}</p>}
                      </div>
                      {address.isDefault && (
                        <span className="text-xs px-2 py-1 rounded-full bg-black text-white">Default</span>
                      )}
                    </div>

                    <div className="flex items-center gap-2 mt-3">
                      {!address.isDefault && (
                        <button
                          onClick={() => handleSetDefault(address.id)}
                          className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-md text-xs border border-gray-300 text-gray-700 hover:bg-gray-50"
                        >
                          <Star className="w-3.5 h-3.5" />
                          Set Default
                        </button>
                      )}
                      <button
                        onClick={() => startEditAddress(address)}
                        className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-md text-xs border border-gray-300 text-gray-700 hover:bg-gray-50"
                      >
                        <Pencil className="w-3.5 h-3.5" />
                        Edit
                      </button>
                      <button
                        onClick={() => handleDeleteAddress(address.id)}
                        className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-md text-xs border border-red-200 text-red-600 hover:bg-red-50"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                        Delete
                      </button>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </div>

          <div className="bg-white border border-gray-200 rounded-2xl p-6 shadow-sm">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              {editingAddressId ? 'Edit Address' : 'Create Address'}
            </h3>

            <form onSubmit={handleSaveAddress} className="space-y-3">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <input
                  placeholder="Recipient name"
                  value={addressForm.recipientName}
                  onChange={(e) => setAddressForm((prev) => ({ ...prev, recipientName: e.target.value }))}
                  className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
                <input
                  placeholder="Phone number"
                  value={addressForm.phoneNumber}
                  onChange={(e) => setAddressForm((prev) => ({ ...prev, phoneNumber: e.target.value }))}
                  className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
                <input
                  placeholder="Province/City"
                  value={addressForm.province}
                  onChange={(e) => setAddressForm((prev) => ({ ...prev, province: e.target.value }))}
                  className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
                <input
                  placeholder="District"
                  value={addressForm.district}
                  onChange={(e) => setAddressForm((prev) => ({ ...prev, district: e.target.value }))}
                  className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
                <input
                  placeholder="Ward"
                  value={addressForm.ward}
                  onChange={(e) => setAddressForm((prev) => ({ ...prev, ward: e.target.value }))}
                  className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
                <input
                  placeholder="Street"
                  value={addressForm.street}
                  onChange={(e) => setAddressForm((prev) => ({ ...prev, street: e.target.value }))}
                  className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
                />
              </div>

              <textarea
                placeholder="Note (optional)"
                value={addressForm.note}
                rows={2}
                onChange={(e) => setAddressForm((prev) => ({ ...prev, note: e.target.value }))}
                className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:border-black"
              />

              <label className="inline-flex items-center gap-2 text-sm text-gray-700">
                <input
                  type="checkbox"
                  checked={addressForm.isDefault}
                  onChange={(e) => setAddressForm((prev) => ({ ...prev, isDefault: e.target.checked }))}
                />
                Set as default address
              </label>

              {addressMessage && <p className="text-sm text-gray-600">{addressMessage}</p>}

              <div className="flex items-center gap-2">
                <button
                  type="submit"
                  disabled={!addressFormValid || savingAddress}
                  className="inline-flex items-center gap-2 gradient-btn text-white px-4 py-2 rounded-lg text-sm font-semibold disabled:opacity-60"
                >
                  <Save className="w-4 h-4" />
                  {savingAddress ? 'Saving...' : editingAddressId ? 'Update Address' : 'Create Address'}
                </button>
                {editingAddressId && (
                  <button
                    type="button"
                    onClick={startCreateAddress}
                    className="px-4 py-2 rounded-lg text-sm border border-gray-300 text-gray-700 hover:bg-gray-50"
                  >
                    Cancel Edit
                  </button>
                )}
              </div>
            </form>
          </div>
        </section>
      </div>
    </div>
  );
}
